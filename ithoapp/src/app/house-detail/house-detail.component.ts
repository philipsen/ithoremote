import { Component, OnInit } from '@angular/core';
import { HousesService } from '../services/houses.service';
import { House } from '../models/house';
import { ActivatedRoute } from '@angular/router';
import { IthoButton } from '../models/itho-button';

@Component({
  selector: 'app-house-detail',
  templateUrl: './house-detail.component.html',
  styleUrls: ['./house-detail.component.sass']
})
export class HouseDetailComponent implements OnInit {

  constructor(
    private housesService: HousesService,
    // private remoteCommandService: RemoteCommandService,
    private route: ActivatedRoute
) { }

  house = new House;
  buttons: IthoButton[];

  ngOnInit() {
    this.getHouse();
    this.getButtons();
  }


  getHouse(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.housesService.getHouse(id)
        .subscribe(house => this.house = house);
}

getButtons(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.buttons = this.housesService.getButtons(id);
}
}
