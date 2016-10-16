/*
 * Athene Requirements Center
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ImmutableObject } from './../';

class MockupObject extends ImmutableObject {
    constructor(
        public property1: string,
        public property2: string
    ) {
        super();
    }

    public changeProperty1(newValue: string): MockupObject {
        return super.cloned<MockupObject>((x: MockupObject) => x.property1 = newValue);
    }

    public clone(): ImmutableObject {
        return new MockupObject(this.property1, this.property2);
    }
}

describe('ImmutableObject', () => {
    it('should create new instance on update', () => {
        const oldObj = new MockupObject('old1', 'old2');
        const newObj = oldObj.changeProperty1('new1');

        expect(oldObj.property1).toBe('old1');
        expect(oldObj.property2).toBe('old2');

        expect(newObj.property1).toBe('new1');
        expect(newObj.property2).toBe('old2');
    });
});
