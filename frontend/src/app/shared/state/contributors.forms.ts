/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed } from '@angular/core';
import { required } from '@angular/forms/signals';
import { SignalForm, Types } from '@app/framework';
import { AssignContributorDto, UserDto } from '../model';

export type AssignContributorFormModel = { user: string | UserDto; role: string };

export class AssignContributorForm extends SignalForm<AssignContributorFormModel, AssignContributorDto, Partial<AssignContributorFormModel>> {
    public readonly hasNoUser = computed(() => {
        return !this.model().user;
    });

    constructor() {
        super({ user: '', role: '' }, path => {
            required(path.user);
            required(path.role);
        });
    }

    protected transformSubmit(value: AssignContributorFormModel) {
        let contributorId = value.user;

        if (Types.is(contributorId, UserDto)) {
            contributorId = contributorId.id;
        }

        return new AssignContributorDto({ contributorId, role: value.role, invite: true });
    }
}

export class ImportContributorsForm extends SignalForm<{ import: string }, ReadonlyArray<AssignContributorDto>> {
    public readonly numberOfEmails = computed(() => {
        return extractEmails(this.model().import).length;
    });

    public readonly hasNoUser = computed(() => {
        return this.numberOfEmails() === 0;
    });

    constructor() {
        super({ import: '' }, path => {
            required(path.import);
        });
    }

    protected transformSubmit(value: { import: string }) {
        return extractEmails(value.import);
    }
}

function extractEmails(value: string) {
    const result: AssignContributorDto[] = [];

    if (value) {
        const added: { [email: string]: boolean } = {};

        const emails = value.match(EMAIL_REGEX);
        if (emails) {
            for (const match of emails) {
                if (!added[match]) {
                    result.push(new AssignContributorDto({ contributorId: match, role: 'Editor', invite: true }));

                    added[match] = true;
                }
            }
        }
    }

    return result;
}


const EMAIL_REGEX = /(?=.{1,254}$)(?=.{1,64}@)[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+(\.[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+)*@[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*/gim;
