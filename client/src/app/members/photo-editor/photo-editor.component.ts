import { Component, Input, OnInit } from '@angular/core';
import { Member } from 'src/app/_models/member';
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';
import { AccountService } from 'src/app/_services/account.service';
import { take } from 'rxjs';
import { IUser } from 'src/app/_models/user';
import { Photo } from 'src/app/_models/photo';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() member: Member | undefined;
  uploader: FileUploader | undefined;
  hasBaseDropZoneOver = false;
  // hasAnotherDropZoneOver = false;
  baseUrl = environment.apiUrl;
  user: IUser | undefined;

  constructor(private accountService: AccountService, private memberService: MembersService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) this.user = user
      }
    });
  }

  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(e: any):void {
    this.hasBaseDropZoneOver = e;
  }

  // fileOverAnother(e: any):void {
  //   this.hasAnotherDropZoneOver = e;
  // }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      // Since this is outside the angular http request, the http interceptor is not going to work here.
      // Hence, we need to manually include the bearer token.
      authToken: 'Bearer ' + this.user?.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024,
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false;
    }

    // After the file has been added successfully.
    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const photo = JSON.parse(response);
        this.member?.photos.push(photo);

        // Code to update the user and member observables on new user's first photo upload.
        if (photo.isMain && this.user && this.member) {
          // If the photo uploaded is the first photo of a user, the API automatically makes it the main photo.
          // Any other photo won't pass the photo.isMain test.
          this.user.photoUrl = photo.url;
          this.member.photoUrl = photo.url;
          this.accountService.setCurrentUser(this.user);
        }
      }
    }
  }

  setMainPhoto(photo: Photo) {
    // The subscribe() function kicks in after the server response from the http.put() method is received.
    // Once the server has been updated, the local variables user and member need to be updated with the main photo as well.
    this.memberService.setMainPhoto(photo.id).subscribe({
      next: () => {
        if (this.user && this.member) {
          this.user.photoUrl = photo.url;
          this.accountService.setCurrentUser(this.user);

          this.member.photoUrl = photo.url;
          this.member.photos.forEach(p => {
            if (p.isMain) p.isMain = false;
            if (p.id == photo.id) p.isMain = true;
          });
        }
      }
    });
  }

  deletePhoto(photoId: number) {
    this.memberService.deletePhoto(photoId).subscribe({
      next: () => {
        if (this.member) {
          this.member.photos = this.member.photos.filter(x => x.id != photoId);
        }
      }
    })
  }
}
