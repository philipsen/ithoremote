import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HousesListComponent } from './houses-list/houses-list.component';
import { HouseDetailComponent } from './house-detail/house-detail.component';
import { HousesAdminComponent } from './houses-admin/houses-admin.component';

const routes: Routes = [
  { path: '', redirectTo: '/houses', pathMatch: 'full' },
  { path: 'houses', component: HousesListComponent },
  { path: 'house/:id', component: HouseDetailComponent },
  { path: 'admin',  component: HousesAdminComponent },
  // { path: 'admin/house/:id',  component: AdminHouseComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
