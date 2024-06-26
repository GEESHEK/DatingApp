import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';
import { map, of, take } from 'rxjs';
import { PaginatedResult } from '../_models/pagination';
import { UserParams } from '../_models/userParams';
import { AccountService } from './account.service';
import { User } from '../_models/user';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;
  members: Member[] = [];
  memberCache = new Map();
  user: User | undefined;
  userParams: UserParams | undefined;

  constructor(private http: HttpClient, private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) {
          this.userParams = new UserParams(user);
          this.user = user;
        }
      }
    })
  }

  getUserParams() {
    return this.userParams;
  }

  setUserParams(params: UserParams) {
    this.userParams = params;
  }

  resetUserParams() {
    if (this.user) {
      this.userParams = new UserParams(this.user);
      return this.userParams;
    }
    return; //if we don't have user object return nothing
  }


  getMembers(UserParams: UserParams) {
    //to check if this query has been made before with the key (these params)
    const response = this.memberCache.get(Object.values(UserParams).join('-'));
    //result will be in response if key match
    if (response) return of(response);

    //allows us to send up the query params
    let params = getPaginationHeaders(UserParams.pageNumber, UserParams.pageSize);

    params = params.append('minAge', UserParams.minAge);
    params = params.append('maxAge', UserParams.maxAge);
    params = params.append('gender', UserParams.gender);
    params = params.append('orderBy', UserParams.orderBy);
    //set result back in the member Cache
    return getPaginatedResult<Member[]>(this.baseUrl + 'users', params, this.http).pipe(
      map(response => {
        this.memberCache.set(Object.values(UserParams).join('-'), response);
        return response; //return results so component can use it
      })
    );
  }

  getMember(username: string) {
    //check the members list to see if we have the member
    // const member = this.members.find(x => x.userName === username);
    // if (member) return of(member);

    const member = [...this.memberCache.values()]
    .reduce((arr, elem) => arr.concat(elem.result), [])
    .find((member: Member) => member.userName === username);

    if (member) return of(member);

    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      map(() => {
        const index = this.members.indexOf(member);
        //update the member in the array with the new updated values
        this.members[index] = { ...this.members[index], ...member }
      })
    );
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {})
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId)
  }

  //post but we are passing up empty object {}
  addLike(username: string) {
    return this.http.post(this.baseUrl + 'likes/' + username, {})
  }

  //not setting up http params for this query string because we only need one
  getLikes(predicate: string, pageNumber: number, pageSize: number) {
    let params = getPaginationHeaders(pageNumber, pageSize);

    params = params.append('predicate', predicate);

    return getPaginatedResult<Member[]>(this.baseUrl + 'likes', params, this.http);
  }

}
