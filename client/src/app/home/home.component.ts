import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  registerMode = false;
  users: any;

  constructor() { }

  ngOnInit(): void {
    this.getUsers();
  }

  registerToggle() {
    this.registerMode = !this.registerMode;
  }

  getUsers() {
    // get() method returns an observable. Hence subscribe() method is used.
    // this.http.get('https://localhost:5005/api/users').subscribe({
    //   next: response => this.users = response,
    //   error: error => console.log(error),
    //   complete: () => console.log('Request has completed.')
    // });
  }

  cancelRegisterMode(event: boolean) {
    this.registerMode = event;
  }
}
