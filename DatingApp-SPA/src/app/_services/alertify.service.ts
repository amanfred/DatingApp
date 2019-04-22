import { Injectable } from '@angular/core';
declare let alertify: any;

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {

constructor() { }

// Are you sure you want to do thid?
confirm (message: string, okCallback: () => any) {
  alertify.confirm(message, function (e: any) {
    if (e) { // e represents the user clicking hte ok button. This is button's click event.
      okCallback(); // this is just a wrapper. We'll be providing a callback function.
    } else {}
  });
}

success(message: string) {
  alertify.success(message);
}

error(message: string) {
  alertify.error(message);
}

warning(message: string) {
  alertify.warning(message);
}

message(message: string) {
  alertify.message(message);
}

}
