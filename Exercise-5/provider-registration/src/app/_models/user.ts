import { EmailValidator } from '@angular/forms';

export class User {
    id: string;
    username: string;
    password: string;
    firstName: string;
    lastName: string;
    npi:number;
    address1:string;
    address2:string;
    city:string;
    state:string;
    zip:string;
    phone:string;
    email:string;
    token: string;
}