import { Component, OnInit, ViewChild } from '@angular/core';
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

  @ViewChild('target', { static: true}) targetElement: any;
  result: string;

  constructor(
    private housesService: HousesService,
    // private remoteCommandService: RemoteCommandService,
    private route: ActivatedRoute
) {
  const id = this.route.snapshot.paramMap.get('id');
  this.housesService.startSubscription(id);
}

  house = new House;
  buttons: IthoButton[];
  public canvasWidth = 500;
  public needleValue = this.housesService.fanspeed;
  public centralLabel = '';
  public name = this.housesService.state;
  public bottomLabel = '';
  // public bottomLabel = '65'
  public options = {
      hasNeedle: true,
      needleColor: 'gray',
      needleUpdateSpeed: 2000,
      arcColors: ['rgb(44, 151, 222)', 'lightgray'],
      arcDelimiters: [30],
      rangeLabel: ['off', 'max'],
      needleStartValue: 0,
  };
  ngOnInit() {
    console.log('init component');
    const width = Math.min(1500, this.targetElement.nativeElement.offsetWidth);
    this.canvasWidth = width - 10;

    this.getHouse();
    this.getButtons();
    setInterval(() => {
      this.canvasWidth = Math.min(1500, this.targetElement.nativeElement.offsetWidth - 10);
      this.options.needleStartValue = this.needleValue.valueOf();
      this.needleValue = this.housesService.fanspeed;
      this.centralLabel = this.needleValue.toString() + '%';
      this.bottomLabel = this.housesService.state;
    }, 1000);
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
