/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { UpdateRoleDto } from '../model';
import { EditRoleForm } from './roles.forms';

describe('EditRoleForm', () => {
    let form: EditRoleForm;

    beforeEach(() => {
        form = TestBed.runInInjectionContext(() => new EditRoleForm());
    });

    it('should load permissions', () => {
        form.load({ permissions: ['a', 'b'] });

        expect(form.model()).toEqual({ permissions: ['a', 'b'] });
    });

    it('should add and remove permissions', () => {
        form.load({ permissions: ['a', 'b'] });
        form.addPermission();
        form.removePermission(0);

        expect(form.model()).toEqual({ permissions: ['b', ''] });
    });

    it('should not submit if permission is empty', () => {
        form.addPermission();

        expect(form.form().valid()).toBeFalsy();
        expect(form.form.permissions[0]().errors().map(x => x.kind)).toEqual(['required']);
    });

    it('should submit permissions', () => {
        form.load({ permissions: ['a'] });

        const result = form.getValue();

        expect(result).toEqual(new UpdateRoleDto({ permissions: ['a'], properties: {} }));
    });
});
