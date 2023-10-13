import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  // Parent to child communication: Getting the users list from the home component.
  // @Input() usersFromHomeComponent: any;
  // Child to parent communication: Emitting the cancel register value to home component.
  @Output() cancelRegister = new EventEmitter();
  // model: any = {}
  // Chapter 143: Reactive Forms introduction.
  registerForm: FormGroup = new FormGroup({});
  maxDate: Date = new Date();
  validationErrors: string[] | undefined;

  constructor(private accountService: AccountService, private toastr: ToastrService, 
    private fb: FormBuilder, private router: Router) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [
        Validators.required,
        Validators.minLength(4),
        Validators.maxLength(8)
      ]],
      confirmPassword: ['', [
        Validators.required,
        this.matchValues('password')
      ]]
    });
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: () => this.registerForm.controls['confirmPassword'].updateValueAndValidity()
    })
  }

  matchValues(matchTo: string): ValidatorFn {
    // Every control is of the type AbstractControl in Reactive forms.
    // Compare the value of the current control to the value of the other control whose name is passed as an argument (matchTo).
    return (control: AbstractControl) => {
      // If the values of the two fields match, return null. Otherwise, return an object with the property notMatching = true.
      return control.value == control.parent?.get(matchTo)?.value ? null : {notMatching: true}
    };
  }

  register() {
    const dob = this.getDateOnly(this.registerForm.controls['dateOfBirth'].value);
    
    // Set the dateOfBirth property of the registerForm to dob by using the spread operator.
    const values = {...this.registerForm.value, dateOfBirth: dob};

    // console.log(values);
    // console.log(this.registerForm.value);

    // this.model.username = this.registerForm?.get('uesrname')?.value;
    // this.model.password = this.registerForm?.get('password')?.value;

    this.accountService.register(values).subscribe({
      // Use () if the response data is not going to be used
      next: () => {
        this.router.navigateByUrl('/members');
      },
      error: error => {
        this.validationErrors = error;
        // this.toastr.error(error.error);
      }     
    })
  }

  cancel() {
    this.cancelRegister.emit(false);
  }

  // Extract just the date from the timezone information returned by the browser.
  private getDateOnly(dob: string | undefined) {
    if (!dob) return;

    let newDob = new Date(dob);
    // Chapter 152 - Client side registration.
    return new Date(newDob.setMinutes(newDob.getMinutes()-newDob.getTimezoneOffset()))
      .toISOString().slice(0, 10);
  }
}
