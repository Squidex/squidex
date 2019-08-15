export class LoginData {
    constructor(uname: string, pwd: string) {
        this.username = uname;
        this.password = pwd;
    }

    public username: string;
    public password: string;
}
