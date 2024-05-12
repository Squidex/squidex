/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

/* eslint-disable no-useless-escape */

import { UntypedFormControl, Validators } from '@angular/forms';
import { map, shareReplay } from 'rxjs/operators';
import { ExtendedFormGroup, Form, value$ } from '@app/framework';
import { AuthSchemeDto } from '@app/shared';

export class UpdateTeamAuthForm extends Form<ExtendedFormGroup, AuthSchemeDto, AuthSchemeDto> {
    public get domain() {
        return this.form.controls['domain'];
    }

    public domainValue$ = value$(this.domain).pipe(
        map(x => x || 'domain.com'));

    public url = value$(this.form).pipe(
        map(x => {
            const q = new URLSearchParams({
                domain: x.domain,
                displayName: x.displayName,
                clientId: x.clientId,
                clientSecret: x.clientSecret,
                authority: x.authority,
            });

            return q.toString();
        }),
        shareReplay(1));

    constructor() {
        super(new ExtendedFormGroup({
            domain: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(40),
            ]),
            displayName: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(40),
            ]),
            clientId: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(200),
            ]),
            clientSecret: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(200),
            ]),
            authority: new UntypedFormControl('', [
                Validators.required,
                Validators.maxLength(200),
            ]),
            signoutRedirectUrl: new UntypedFormControl('', [
                Validators.maxLength(500),
            ]),
        }));

        this.url.subscribe();
    }
}

