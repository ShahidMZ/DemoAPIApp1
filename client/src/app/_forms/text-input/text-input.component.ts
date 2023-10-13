import { Component, Input, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl } from '@angular/forms';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  styleUrls: ['./text-input.component.css']
})
export class TextInputComponent implements ControlValueAccessor {
  // The ControlValueAccessor is used to create a custom form control that integrates with Angular forms.
  @Input() label = '';
  @Input() type = 'text';

  // If we inject something in the constructor, Angular will check if it has been used recently. If yes, it'll be reloaded from the memory and reused.
  // When it comes to custom input controls, we don't want to reuse a control that is already in the memory. 
  // The @Self() decorator is used to ensure that the ngControl being injected here is a new control.
  constructor(@Self() public ngControl: NgControl) {
    // "this" represents the current class.
    this.ngControl.valueAccessor = this;
  }

  writeValue(obj: any): void { }

  registerOnChange(fn: any): void { }

  registerOnTouched(fn: any): void { }

  // Cast this AbstractControl as a FormControl
  get control(): FormControl {
    return this.ngControl.control as FormControl;
  }
}
