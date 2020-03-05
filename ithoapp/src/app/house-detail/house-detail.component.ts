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
  public canvasWidth = 500;
  public needleValue = this.housesService.fanspeed;
  public centralLabel = 'eco';
  public name = this.housesService.state;
  // public bottomLabel = '65'
  public options = {
      hasNeedle: true,
      needleColor: 'gray',
      needleUpdateSpeed: 3000,
      arcColors: ['rgb(44, 151, 222)', 'lightgray'],
      arcDelimiters: [30],
      rangeLabel: ['0', '100'],
      needleStartValue: 50,
  };
  ngOnInit() {
    this.getHouse();
    this.getButtons();
    setInterval(() => {
      this.options.needleStartValue = this.needleValue.valueOf();
      this.needleValue = this.housesService.fanspeed;
      this.centralLabel = this.housesService.state;
    }, 2000);
  }

  getHouse(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.house.name = id;
}

getButtons(): void {
    const id = this.route.snapshot.paramMap.get('id');
    this.buttons = this.housesService.getButtons(id);
}
}
