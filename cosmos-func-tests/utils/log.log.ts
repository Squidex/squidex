//this class is under development

export class Log{

    constructor(){}
    logging(){
    var winston = require('winston');
    winston.remove(winston.transports.Console);
    winston.add(winston.transports.Console, {timestamp : true});
    winston.add(winston.transports.File, {filename : 'winston-basic.log'});
    module.exports = winston;
    }
}