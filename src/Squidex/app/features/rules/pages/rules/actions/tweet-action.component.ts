/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';

import { DialogService } from '@app/shared';

@Component({
    selector: 'sqx-tweet-action',
    styleUrls: ['./tweet-action.component.scss'],
    templateUrl: './tweet-action.component.html'
})
export class TweetActionComponent implements OnInit {
    private request: any;

    @Input()
    public action: any;

    @Input()
    public actionForm: FormGroup;

    @Input()
    public actionFormSubmitted = false;

    public isAuthenticating = false;
    public isRedirected = false;

    public pinCode: string;

    constructor(
        private readonly dialogs: DialogService,
        private readonly httpClient: HttpClient
    ) {
    }

    public ngOnInit() {
        this.actionForm.setControl('accessToken',
            new FormControl(this.action.accessToken || '', [
                Validators.required
            ]));

        this.actionForm.setControl('accessSecret',
            new FormControl(this.action.accessSecret || '', [
                Validators.required
            ]));

        this.actionForm.setControl('text',
            new FormControl(this.action.text || '', [
                Validators.required,
                Validators.maxLength(280)
            ]));
    }

    public auth() {
        this.isAuthenticating = true;

        this.httpClient.get('api/rules/twitter/auth')
            .subscribe((response: any) => {
                this.request = {
                    requestToken: response.requestToken,
                    requestTokenSecret: response.requestTokenSecret
                };

                this.isAuthenticating = false;
                this.isRedirected = true;

                window.open(response.authorizeUri, '_blank');
            }, () => {
                this.dialogs.notifyError('Failed to authenticate with twitter.');

                this.isAuthenticating = false;
                this.isRedirected = false;
            });
    }

    public complete() {
        this.request.pinCode = this.pinCode;

        this.httpClient.post('api/rules/twitter/token', this.request)
            .subscribe((response: any) => {
                this.actionForm.get('accessToken')!.setValue(response.accessToken);
                this.actionForm.get('accessSecret')!.setValue(response.accessTokenSecret);

                this.isRedirected = false;
            }, () => {
                this.dialogs.notifyError('Failed to request access token.');

                this.isAuthenticating = false;
                this.isRedirected = false;
            });
    }
}