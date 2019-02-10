import { Component, OnInit, Input } from '@angular/core';
import { IthoButton } from 'src/app/models/itho-button';
import { HousesService } from 'src/app/services/houses.service';

@Component({
  selector: 'app-itho-button',
  templateUrl: './itho-button.component.html',
  styleUrls: ['./itho-button.component.sass']
})
export class IthoButtonComponent implements OnInit {

  @Input() button: IthoButton;
  @Input() house: string;

  constructor(
    private housesService: HousesService,
    // private remoteCommandService: RemoteCommandService
    ) { }

  ngOnInit() {
  }


  // sendCommandBytes(remoteId: string, remoteCommand: string): void {
  //   this.remoteCommandService.sendCommandBytes(this.house, remoteId, remoteCommand).subscribe(() => {
  //   });
  // }

}
