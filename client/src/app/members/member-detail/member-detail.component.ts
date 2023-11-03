import { CommonModule } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CarouselModule } from 'ngx-bootstrap/carousel';
import { TabDirective, TabsModule, TabsetComponent } from 'ngx-bootstrap/tabs';
// import { GalleryModule, GalleryItem, ImageItem } from 'ng-gallery';
import { Member } from 'src/app/_models/member';
import { Photo } from 'src/app/_models/photo';
import { MembersService } from 'src/app/_services/members.service';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';
import { MessageService } from 'src/app/_services/message.service';
import { Message } from 'src/app/_models/message';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'app-member-detail',
    standalone: true,
    templateUrl: './member-detail.component.html',
    styleUrls: ['./member-detail.component.css'],
    // Import modules here for standalone components.
    imports: [CommonModule, TabsModule, CarouselModule, MemberMessagesComponent]
})
export class MemberDetailComponent implements OnInit {
    // When a template reference variable is used in the HTML component (like #memberTabs) @ViewChild can be used to access it.
    @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent;
    member: Member = {} as Member;
    // Before route resolver was added:
    // member: Member | undefined;
    photos: Photo[] = [];
    // images: GalleryItem[] = [];
    activeTab?: TabDirective;
    messages: Message[] = [];

    // route: ActivatedRoute gives us access to the "username" route parameter defined in app-routing.module.ts file -> path: 'members/:username'
    constructor(private memberService: MembersService, private route: ActivatedRoute,
        private messageService: MessageService, private toastr: ToastrService) { }

    ngOnInit(): void {
        // this.loadMember();
        
        // Use route resolver to populate the member property instead of using the loadMember() method.
        this.route.data.subscribe({
            // 'member' is the name defined in the app-routing module in the MemberDetailComponent's resolve property.
            next: data => this.member = data['member']
        });

        // Using query parameters, if present, navigate to the selected tab.
        // query parameters example: https.//localhost:4200/members/ben?tab=Messages
        this.route.queryParams.subscribe({
            next: params => {
                // params['tab'] && this.selectTab(params['tab']);
                if (params['tab'])
                {
                    this.selectTab(params['tab']);
                }
            }
        });

        this.getPhotos();
    }

    selectTab(heading: string) {
        if (this.memberTabs) {
            // Use the ! operator after find() to remove the optional property access error.
            this.memberTabs.tabs.find(x => x.heading == heading)!.active = true;
        }
    }

    onTabActivated(data: TabDirective) {
        this.activeTab = data;
        if (this.activeTab.heading == 'Messages') {
            this.loadMessages();
        }
    }

    loadMessages() {
        if (this.member) {
            this.messageService.getMessageThread(this.member.userName).subscribe({
                next: messageThread => this.messages = messageThread
            });
        }
    }

    getPhotos() {
        if (!this.member) return;

        for (const photo of this.member.photos) {
            this.photos.push(photo);
        }
    }

    addLike(mem: Member) {
        this.memberService.addLike(mem.userName).subscribe({
            next: () => this.toastr.success("You have liked " + mem.knownAs)
        });
    }

    // Implemented in member-detailed route resolver. Chapter 196: Using route resolvers
    // loadMember() {
    //     const username = this.route.snapshot.paramMap.get('username');
    //     if (!username) return;
    //     this.memberService.getMember(username).subscribe({
    //         next: member => {
    //             this.member = member;
    //             // this.getImages();
    //             this.getPhotos();
    //         }
    //     });
    // }

    // getImages() {
    //   if (!this.member) return;
    //   for (const photo of this.member.photos) {
    //     this.images.push(new ImageItem({
    //       src: photo.url,
    //       thumb: photo.url
    //     }))
    //   }
    // }
}
