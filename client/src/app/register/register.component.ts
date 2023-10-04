import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // Parent to child communication: Getting the users list from the home component.
  // @Input() usersFromHomeComponent: any;
  // Child to parent communication: Emitting the canel register value to home component.
  @Output() cancelRegister = new EventEmitter();
  model: any = {}

  constructor(private accountService: AccountService, private toastr: ToastrService) { }

  ngOnInit(): void {
  }

  register() {
    this.accountService.register(this.model).subscribe({
      // Use () if the response data is not going to be used
      next: () => {
        this.cancel();
      },
      error: error => {
        console.log(error);
        this.toastr.error(error.error);
      }     
    })
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
