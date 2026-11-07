/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { AssignContributorDto } from '../model';
import { AssignContributorForm, ImportContributorsForm } from './contributors.forms';

describe('AssignContributorForm', () => {
    let form: AssignContributorForm;

    beforeEach(() => {
        form = TestBed.runInInjectionContext(() => new AssignContributorForm());
    });

    it('should have no user initially', () => {
        expect(form.hasNoUser()).toBeTruthy();
    });

    it('should submit contributor', () => {
        form.load({ user: 'user1', role: 'Editor' });

        const result = form.getValue();

        expect(form.hasNoUser()).toBeFalsy();
        expect(result).toEqual(new AssignContributorDto({ contributorId: 'user1', role: 'Editor', invite: true }));
    });

    it('should not submit without role', () => {
        form.load({ user: 'user1' });

        expect(form.form().valid()).toBeFalsy();
    });
});

describe('ImportContributorsForm', () => {
    let form: ImportContributorsForm;

    beforeEach(() => {
        form = TestBed.runInInjectionContext(() => new ImportContributorsForm());
    });

    it('should count unique emails', () => {
        form.form.import().value.set('a@squidex.io;b@squidex.io a@squidex.io');

        expect(form.numberOfEmails()).toBe(2);
        expect(form.hasNoUser()).toBeFalsy();
    });

    it('should have no user if no email detected', () => {
        form.form.import().value.set('invalid');

        expect(form.numberOfEmails()).toBe(0);
        expect(form.hasNoUser()).toBeTruthy();
    });
});
