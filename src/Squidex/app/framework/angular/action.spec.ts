/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Subject } from 'rxjs';

import { Action } from './../';

class MockupObject {
    public isDestroyCalled = false;

    @Action()
    public event1 = new Subject<string>().map(x => { return { type: 'MOCK_ACTION' }; });

    @Action()
    public event2 = new Subject<string>().map(x => { return { type: 'MOCK_ACTION' }; });

    constructor(private readonly store: any) { }

    public init() {
        this.event2 = new Subject<string>().map(x => { return { type: 'MOCK_ACTION' }; });
    }

    public ngOnDestroy() {
        this.isDestroyCalled = true;
    }
}

describe('Action', () => {
    it('should test complete flow to subscribe and unsubscribe and to trigger actions', () => {
        let dispatchCount = 0;

        const state = {
            next: (e: any) => {
                dispatchCount++;

                expect(e.type).toBe('MOCK_ACTION');
            }
        };

        const mock = new MockupObject(state);

        mock.init();

        (<any>mock.event1).next('TEST');
        (<any>mock.event2).next('TEST');

        expect(dispatchCount).toBe(2);

        mock.ngOnDestroy();

        (<any>mock.event1).next('TEST');
        (<any>mock.event2).next('TEST');

        expect(dispatchCount).toBe(2);
        expect(mock.isDestroyCalled).toBeTruthy();
    });
});