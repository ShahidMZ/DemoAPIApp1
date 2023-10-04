import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { ListsComponent } from './lists/lists.component';
import { MessagesComponent } from './messages/messages.component';
import { AuthGuard } from './_guards/auth.guard';

const routes: Routes = [
  // Chapter 66
  // Loads app home when no route is present.
  {path: '', component: HomeComponent},
  // Protect all children under AuthGuard
  {path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [AuthGuard],
    children: [
      // Matches link.
      {path: 'members', component: MemberListComponent},
      // ':id' represents a route parameter.
      {path: 'members/:id', component: MemberDetailComponent},
      // Lists link.
      {path: 'lists', component: ListsComponent},
      // Messages link.
      {path: 'messages', component: MessagesComponent}
    ]
  },
  // '**' represents an invalid route.
  {path: '**', component: HomeComponent, pathMatch: 'full'}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
