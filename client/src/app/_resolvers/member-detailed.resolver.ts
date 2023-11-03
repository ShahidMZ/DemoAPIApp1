import { ResolveFn } from "@angular/router";
import { Member } from "../_models/member";
import { MembersService } from "../_services/members.service";
import { inject } from "@angular/core";

export const MemberDetailedResolver: ResolveFn<Member> = (route, state) => {
  const memberService = inject(MembersService);

  return memberService.getMember(route.paramMap.get('username')!);
};

// @Injectable({
//   providedIn: 'root'
// })
// export class MemberDetailedResolver implements Resolve<boolean> {
//   resolve(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
//     return of(true);
//   }
// }
