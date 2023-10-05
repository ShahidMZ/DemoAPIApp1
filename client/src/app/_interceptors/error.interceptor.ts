import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { Observable, catchError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router, private toastr: ToastrService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Chapter 80.
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error) {
          switch (error.status) {
            // 400 Validation error and 400 Bad Request error.
            case 400:
              // Check if a 400 Validation error is present.
              // This type of error has username and password errors in the 'errors' object of an 'error' object.
              // If the http response has these objects, it means it is a 400 Validation error.
              if (error.error.errors) {
                // The errors object has either one or both of the username and password errors.
                // Store them in an array and then throw it.
                const modelStateErrors = [];
                for (const key in error.error.errors) {
                  if (error.error.errors[key]) {
                    modelStateErrors.push(error.error.errors[key]);
                  }
                }
                throw modelStateErrors.flat();
              } else {
                // 400 Bad Request error
                this.toastr.error(error.error, error.status.toString());
              }
              break;
            case 401:
              this.toastr.error('Unauthorized', error.status.toString());
              break;
            case 404:
              // Redirect to a "Not found" page.
              this.router.navigateByUrl('/not-found');
              break;
            case 500:
              // Give the server-error component access to the error response so that the error information can be displayed in the component.
              // Store the error in the router state. After navigating to the server-error page, we will have access to the error in the navigationExtras object.
              const navigationExtras: NavigationExtras = { state: {error: error.error }};
              this.router.navigateByUrl('/server-error', navigationExtras);
              break;
            default:
              this.toastr.error("Something unexpected went wrong.");
              console.log(error);
              break;
          }
        }
        throw error;
      })
    );
  }
}
