/*
 *PinkParrot CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Observable } from 'rxjs/Observable';

export abstract class AppStore {
    public abstract select<T>(pathOrMapFn: any, ...paths: string[]): Observable<T>;

    public abstract next(action: any): void;
}