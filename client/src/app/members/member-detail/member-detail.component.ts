import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CarouselModule } from 'ngx-bootstrap/carousel';
import { TabsModule } from 'ngx-bootstrap/tabs';
// import { GalleryModule, GalleryItem, ImageItem } from 'ng-gallery';
import { Member } from 'src/app/_models/member';
import { Photo } from 'src/app/_models/photo';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
  // Import modules here for standalone components.
  imports: [CommonModule, TabsModule, CarouselModule]
})
export class MemberDetailComponent implements OnInit {
  member: Member | undefined;
  photos: Photo[] = [];
  // images: GalleryItem[] = [];

  // route: ActivatedRoute gives us access to the "username" route parameter defined in app-routing.module.ts file -> path: 'members/:username'
  constructor(private memberService: MembersService, private route: ActivatedRoute) { }

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    const username = this.route.snapshot.paramMap.get('username');
    if (!username) return;
    this.memberService.getMember(username).subscribe({
      next: member => {
        this.member = member;
        // this.getImages();
        this.getPhotos();
      }
    });
  }

  getPhotos() {
    if (!this.member) return;

    for (const photo of this.member.photos) {
      this.photos.push(photo);
    }
  }

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
