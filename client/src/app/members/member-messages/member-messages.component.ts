import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { TimeagoModule } from 'ngx-timeago';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  imports: [CommonModule, TimeagoModule]
})
export class MemberMessagesComponent implements OnInit {
  //child, get username&messages from parent with Input
  @Input() username?: string;
  @Input() messages: Message[] = [];

  constructor() { }

  ngOnInit(): void {
  }



}
