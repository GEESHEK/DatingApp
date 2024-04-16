import { ResolveFn } from '@angular/router';
import { Member } from '../_models/member';
import { MembersService } from '../_services/members.service';
import { inject } from '@angular/core';

export const memberDetailedResolver: ResolveFn<Member> = (route, state) => {
  const memberService = inject(MembersService);

  //get access to route with the paramMaps to get the username 
  //'members/:username' in the app-routing module
  return memberService.getMember(route.paramMap.get('username')!)
};
