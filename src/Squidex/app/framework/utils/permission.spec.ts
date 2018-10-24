/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Permission, permissionsAllow } from './permission';

describe('Permission', () => {
    it('Should_return_true_if_given_and_requested_permission_have_wildcards', () => {
        const g = new Permission('app.*');
        const r = new Permission('app.*');

        expect(g.allows(r)).toBeTruthy();
    });

    it('Should_return_true_if_given_permission_equals_requested_permission', () => {
        const g = new Permission('app.contents');
        const r = new Permission('app.contents');

        expect(g.allows(r)).toBeTruthy();
    });

    it('Should_return_true_if_given_permission_is_parent_of_requested_permission', () => {
        const g = new Permission('app');
        const r = new Permission('app.contents');

        expect(g.allows(r)).toBeTruthy();
    });

    it('Should_return_true_if_given_permission_is_alternative_of_requested_permission', () => {
        const g = new Permission('app.contents|schemas');
        const r = new Permission('app.contents');

        expect(g.allows(r)).toBeTruthy();
    });

    it('Should_return_true_if_given_permission_equals_alternative_requested_permission', () => {
        const g = new Permission('app.contents');
        const r = new Permission('app.contents|schemas');

        expect(g.allows(r)).toBeTruthy();
    });

    it('Should_return_true_if_given_permission_has_wildcard_for_requested_permission', () => {
        const g = new Permission('app.*');
        const r = new Permission('app.contents');

        expect(g.allows(r)).toBeTruthy();
    });

    it('Should_return_false_if_given_permission_not_equals_requested_permission', () => {
        const g = new Permission('app.contents');
        const r = new Permission('app.assets');

        expect(g.allows(r)).toBeFalsy();
    });

    it('Should_return_false_if_given_permission_is_child_of_requested_permission', () => {
        const g = new Permission('app.contents');
        const r = new Permission('app');

        expect(g.allows(r)).toBeFalsy();
    });

    it('Should_return_false_if_given_permission_has_no_wildcard_but_requested_has', () => {
        const g = new Permission('app.contents');
        const r = new Permission('app.*');

        expect(g.allows(r)).toBeFalsy();
    });

    it('Should_return_false_if_given_requested_permission_is_null', () => {
        const g = new Permission('app.contents');

        expect(g.allows(null!)).toBeFalsy();
    });

    it('Should_return_true_if_any_permission_gives_permission_to_request', () => {
        const sut = [
            new Permission('app.contents'),
            new Permission('app.assets')
        ];

        expect(permissionsAllow(sut, new Permission('app.contents'))).toBeTruthy();
    });

    it('Should_return_false_if_none_permission_gives_permission_to_request', () => {
        const sut = [
            new Permission('app.contents'),
            new Permission('app.assets')
        ];

        expect(permissionsAllow(sut, new Permission('app.schemas'))).toBeFalsy();
    });

    it('Should_return_false_if_permission_to_request_is_null', () => {
        const sut = [
            new Permission('app.contents'),
            new Permission('app.assets')
        ];

        expect(permissionsAllow(sut, null!)).toBeFalsy();
    });
});