import { IUser } from "./user";

export class UserParams {
    gender: string;
    minAge = 18;
    maxAge = 99;
    pageNumber = 1;
    pageSize = 12;
    orderBy = "lastActive";

    constructor(user: IUser) {
        this.gender = user.gender == 'female' ? 'male' : 'female';
    }
}