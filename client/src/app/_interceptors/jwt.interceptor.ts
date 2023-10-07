import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable, take } from 'rxjs';
import { AccountService } from '../_services/account.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {

  constructor(private accountService: AccountService) {}

  // Chapter 109 - Using an Interceptor to send the token.
  // Intercept outgoing requests and add the Bearer tokens as Authorization headers.
  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    // Instead of subscribing to an observable, which will require us to unsubscribe later, 
    // the pipe(take(1)) method can be used to take just the specified number of values from the observable.
    this.accountService.currentUser$.pipe(take(1)).subscribe({
      next: user => {
        if (user) {
          request = request.clone({
            setHeaders: {
              // Alternative method: Using back ticks (`) lets us use ${} to access objects within them.
              Authorization: `Bearer ${user.token}`
            }
          });
        }
      }
    });
    
    return next.handle(request);
  }
}
