import { CommonModule } from '@angular/common';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css'],
  imports: [CommonModule, FormsModule]
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm?: NgForm;
  @Input() messages: Message[] = [];
  @Input() username?: string;
  messageContent = '';

  constructor(private messageService: MessageService) { }

  ngOnInit(): void {

  }

  sendMessage() {
    // If there is no recipient user, do nothing.
    if (!this.username) return;

    this.messageService.sendMessage(this.username, this.messageContent).subscribe({
      next: message => {
        // Add the message to the messages array and reset the form to clear the text.
        this.messages.push(message);
        this.messageForm?.reset();
      }
    });
  }
}
