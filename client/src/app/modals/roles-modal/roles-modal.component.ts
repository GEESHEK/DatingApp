import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-roles-modal',
  templateUrl: './roles-modal.component.html',
  styleUrls: ['./roles-modal.component.css']
})
export class RolesModalComponent implements OnInit {
  //we get these properties from the component that call this component
  username = '';
  availableRoles: any[] = [];
  selectedRoles: any[] = [];

  constructor(public bsModalRef: BsModalRef) {}
  
  ngOnInit(): void {

  }

  updateChecked(checkedValue: string) {
    //index = -1 means not inside selectedRoles array then we add it to the selectedRoles
    const index = this.selectedRoles.indexOf(checkedValue);
    //otherwise we remove it
    index != -1 ? this.selectedRoles.splice(index, 1) : this.selectedRoles.push(checkedValue);

  }
}
