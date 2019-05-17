
export class LoginData {

    constructor(uname:string, pwd:string)
    {
        this.username = uname;
        this.password = pwd;
    }

    username : string;
    password : string;
}

export interface ILoginData {
    name:string;
    username:string;
    password:string;
}

