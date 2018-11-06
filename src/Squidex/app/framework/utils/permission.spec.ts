/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Permission } from './permission';

describe('Permission', () => {
    it('should check when permissions are not equal', () => {
        const g = new Permission('app.contents');
        const r = new Permission('app.assets');

        expect(g.allows(r)).toBeFalsy();

        expect(g.includes(r)).toBeFalsy();
    });

    it('should check when permissions are equal with wildcards', () => {
        const g = new Permission('app.*');
        const r = new Permission('app.*');

        expect(g.allows(r)).toBeTruthy();

        expect(g.includes(r)).toBeTruthy();
    });

    it('should check when equal permissions', () => {
        const g = new Permission('app.contents');
        const r = new Permission('app.contents');

        expect(g.allows(r)).toBeTruthy();

        expect(g.includes(r)).toBeTruthy();
    });

    it('should check when given is parent of requested', () => {
        const g = new Permission('app');
        const r = new Permission('app.contents');

        expect(g.allows(r)).toBeTruthy();

        expect(g.includes(r)).toBeTruthy();
    });

    it('should check when requested is parent of given', () => {
        const g = new Permission('app.contents');
        const r = new Permission('app');

        expect(g.allows(r)).toBeFalsy();

        expect(g.includes(r)).toBeTruthy();
    });

    it('should check when given is wildcard of requested', () => {
        const g = new Permission('app.*');
        const r = new Permission('app.contents');

        expect(g.allows(r)).toBeTruthy();

        expect(g.includes(r)).toBeTruthy();
    });

    it('should check when requested is wildcard of given', () => {
        const g = new Permission('app.contents');
        const r = new Permission('app.*');

        expect(g.allows(r)).toBeFalsy();

        expect(g.includes(r)).toBeTruthy();
    });

    it('should check when given is has alternatives of requested', () => {
        const g = new Permission('app.contents|schemas');
        const r = new Permission('app.contents');

        expect(g.allows(r)).toBeTruthy();

        expect(g.includes(r)).toBeTruthy();
    });

    it('should check when requested is has alternatives of given', () => {
        const g = new Permission('app.contents');
        const r = new Permission('app.contents|schemas');

        expect(g.allows(r)).toBeTruthy();

        expect(g.includes(r)).toBeTruthy();
    });


    it('should check for requested is null', () => {
        const g = new Permission('app.contents');

        expect(g.allows(null!)).toBeFalsy();

        expect(g.includes(null!)).toBeFalsy();
    });

    it('should return true if any permission gives permission to requested', () => {
        const set = [
            new Permission('app.contents'),
            new Permission('app.assets')
        ];

        expect(new Permission('app.contents').allowedBy(set)).toBeTruthy();
    });

    it('should return true if any permission includes parent given', () => {
        const set = [
            new Permission('app.contents'),
            new Permission('app.assets')
        ];

        expect(new Permission('app').includedIn(set)).toBeTruthy();
    });

    it('should return true if any permission includes child given', () => {
        const set = [
            new Permission('app.contents'),
            new Permission('app.assets')
        ];

        expect(new Permission('app.contents.read').includedIn(set)).toBeTruthy();
    });

    it('should return false if none permission gives permission to requested', () => {
        const set = [
            new Permission('app.contents'),
            new Permission('app.assets')
        ];

        expect(new Permission('app.schemas').allowedBy(set)).toBeFalsy();
    });

    it('should return false if none permission includes given', () => {
        const set = [
            new Permission('app.contents'),
            new Permission('app.assets')
        ];

        expect(new Permission('other').allowedBy(set)).toBeFalsy();
    });
});