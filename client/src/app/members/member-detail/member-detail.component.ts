import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TabDirective, TabsModule, TabsetComponent } from 'ngx-bootstrap/tabs';
import { TimeagoModule } from 'ngx-timeago';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';
import { MessageService } from 'src/app/_services/message.service';
import { Message } from 'src/app/_models/message';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
  imports: [CommonModule, TabsModule, GalleryModule, TimeagoModule, MemberMessagesComponent]
})
export class MemberDetailComponent implements OnInit {
  @ViewChild('memberTabs', {static: true}) memberTabs?: TabsetComponent;
  //initializes member with empty object but should be populated by the member-detailed route resolver.
  member: Member = {} as Member;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;
  messages: Message[] = [];

  constructor(private memberService: MembersService, private route: ActivatedRoute, 
    private messageService: MessageService) {
  }

  ngOnInit(): void {
    this.route.data.subscribe({
      //member is what we called it inside the app routing module
      next: data => this.member = data['member']
    })


    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab'])
      }
    })

    this.getImages();
  }

  selectTab(heading: string) {
    if(this.memberTabs) {
      //if we have a memberTabs we know for sure we have the headings > use ! or TS will complain
      this.memberTabs.tabs.find(x => x.heading === heading)!.active = true;
    }
    
  }

  //load the messages when the tab is activated here instead of the message component
  //since the message is a child, the ngOnInit will be called to load the members 
  //even though the user might not click on the tab
  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    //tab heading name
    if (this.activeTab.heading === 'Messages') {
        this.loadMessages();
    }
  }

  loadMessages() {
    if(this.member) {
      this.messageService.getMessageThread(this.member.userName).subscribe({
        next: messages => this.messages = messages
      })
    }
  }

  // loadMember() {
  //   const username = this.route.snapshot.paramMap.get('username');
  //   if (!username) return;

  //   this.memberService.getMember(username).subscribe({
  //     next: member => {
  //       this.member = member,
  //         this.getImages()
  //     }
  //   })
  // }

  getImages() {
    if (!this.member) return;
    for (const photo of this.member?.photos) {
      this.images.push(new ImageItem({ src: photo.url, thumb: photo.url }))
    }
  }

}
