import { Component, OnInit } from '@angular/core';
import { HousesService } from '../services/houses.service';
import { FanState } from '../models/fanstate';

@Component({
  selector: 'app-houses-admin',
  templateUrl: './houses-admin.component.html',
  styleUrls: ['./houses-admin.component.sass']
})
export class HousesAdminComponent implements OnInit {

  fanstates: FanState[];

  constructor(private houseService: HousesService) {
    this.houseService.startFanstateSubscription();
    this.houseService.startTransponderSubscription();
    this.fanstates = this.houseService.fanstates;
  }

  ngOnInit() {
    // setInterval(() => {
    //   this.fanstates = this.houseService.fanstates;
    // }, 1000);
  }

}
