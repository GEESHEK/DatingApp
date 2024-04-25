import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';
import { Message } from '../_models/message';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { User } from '../_models/user';
import { BehaviorSubject, take } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http: HttpClient) { }

  //get the other member name from the member details component using the root parameter
  createHubConnection(user: User, otherUsername: string) {
    this.hubConnection = new HubConnectionBuilder()
      //route comes from the program class on the API
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        //need to authenticate to the message hub as well
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(error => console.log(error));

    this.hubConnection.on('ReceiveMessageThread', messages => {
        this.messageThreadSource.next(messages);
    })

    //replace the array instead of mutating it
    //concat with the observable and add a new one on and replace the existing one
    this.hubConnection.on('NewMessage', message => {
      this.messageThread$.pipe(take(1)).subscribe({
        next: messages => {
          //spread operator replaces existing array 
          //(existing messages, add on new  message from signalR)
          this.messageThreadSource.next([...messages, message])
        }
      })
    })
  }

  stopHubConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);
    return getPaginatedResult<Message[]>(this.baseUrl + 'messages', params, this.http)
  }

  getMessageThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username)
  }

  async sendMessage(username: string, content: string) {
    //recipientUsername match with message DTO, content is fine > called the same
    //no longer use HTTP post, use hub for this
    //invokes a message on our server, on the API hub, method name
    return this.hubConnection?.invoke('SendMessage', { recipientUsername: username, content })
      .catch(error => console.log(error));
    }

    deleteMessage(id: number) {
      return this.http.delete(this.baseUrl + 'messages/' + id);
    }
  }
