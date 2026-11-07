/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed } from '@angular/core';
import { required } from '@angular/forms/signals';
import { AssignContributorDto, SignalForm, Types, UserDto } from '@app/shared';

type AssignTeamContributorFormModel = { user: string | UserDto };

export class AssignTeamContributorForm extends SignalForm<AssignTeamContributorFormModel, AssignContributorDto, { user: string }> {
    public readonly hasNoUser = computed(() => {
        return !this.model().user;
    });

    constructor() {
        super({ user: '' }, path => {
            required(path.user);
        });
    }

    protected transformSubmit(value: AssignTeamContributorFormModel) {
        let contributorId = value.user;

        if (Types.is(contributorId, UserDto)) {
            contributorId = contributorId.id;
        }

        return new AssignContributorDto({ contributorId, role: 'Owner', invite: true });
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
                    result.push(new AssignContributorDto({ contributorId: match, role: 'Owner', invite: true }));
                    added[match] = true;
                }
            }
        }
    }

    return result;
}

const EMAIL_REGEX = /(?=.{1,254}$)(?=.{1,64}@)[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+(\.[-!#$%&'*+\/0-9=?A-Z^_`a-z{|}~]+)*@[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?(\.[A-Za-z0-9]([A-Za-z0-9-]{0,61}[A-Za-z0-9])?)*/gim;
