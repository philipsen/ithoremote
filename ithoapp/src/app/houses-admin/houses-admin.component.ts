import { Component, OnInit } from '@angular/core';
import { HousesService } from '../services/houses.service';
import { FanState } from '../models/fanstate';

@Component({
  selector: 'app-houses-admin',
  templateUrl: './houses-admin.component.html',
  styleUrls: ['./houses-admin.component.sass']
})
export class HousesAdminComponent {

  fanstates: FanState[];

  constructor(public houseService: HousesService) {
    this.houseService.startFanstateSubscription();
    this.houseService.startTransponderSubscription();
    this.fanstates = this.houseService.fanstates;
  }

  formatId(id: Number[]) {
    return id.map(s => s.toString(16)).join(':');
  }
}
