/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed } from '@angular/core';
import { maxLength, required } from '@angular/forms/signals';
import { AuthSchemeDto, SignalForm } from '@app/shared';

export type UpdateTeamAuthFormModel = {
    domain: string;
    displayName: string;
    clientId: string;
    clientSecret: string;
    authority: string;
    signoutRedirectUrl: string;
};

export class UpdateTeamAuthForm extends SignalForm<UpdateTeamAuthFormModel, AuthSchemeDto, Partial<AuthSchemeDto>> {
    public readonly domainValue = computed(() => {
        return this.model().domain || 'domain.com';
    });

    public readonly url = computed(() => {
        const value = this.model();

        const query = new URLSearchParams({
            domain: value.domain,
            displayName: value.displayName,
            clientId: value.clientId,
            clientSecret: value.clientSecret,
            authority: value.authority,
        });

        return query.toString();
    });

    constructor() {
        super({ domain: '', displayName: '', clientId: '', clientSecret: '', authority: '', signoutRedirectUrl: '' }, path => {
            maxLength(path.domain, 40);
            maxLength(path.displayName, 40);
            maxLength(path.clientId, 200);
            maxLength(path.clientSecret, 200);
            maxLength(path.authority, 200);
            maxLength(path.signoutRedirectUrl, 500);
            required(path.domain);
            required(path.displayName);
            required(path.clientId);
            required(path.clientSecret);
            required(path.authority);
        });
    }

    protected transformSubmit(value: UpdateTeamAuthFormModel) {
        return new AuthSchemeDto(value);
    }
}
