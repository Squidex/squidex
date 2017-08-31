/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    CreateWebhookDto,
    DateTime,
    DialogService,
    ImmutableArray,
    SchemaDto,
    SchemasService,
    Version,
    WebhookDto,
    WebhooksService,
    UpdateWebhookDto
} from 'shared';

@Component({
    selector: 'sqx-webhooks-page',
    styleUrls: ['./webhooks-page.component.scss'],
    templateUrl: './webhooks-page.component.html'
})
export class WebhooksPageComponent extends AppComponentBase implements OnInit {
    public webhooks: ImmutableArray<WebhookDto>;
    public schemas: SchemaDto[];

    public addWebhookFormSubmitted = false;
    public addWebhookForm =
        this.formBuilder.group({
            url: ['',
                [
                    Validators.required
                ]]
        });

    public get hasUrl() {
        return this.addWebhookForm.controls['url'].value && this.addWebhookForm.controls['url'].value.length > 0;
    }

    constructor(apps: AppsStoreService, dialogs: DialogService,
        private readonly authService: AuthService,
        private readonly schemasService: SchemasService,
        private readonly webhooksService: WebhooksService,
        private readonly formBuilder: FormBuilder
    ) {
        super(dialogs, apps);
    }

    public ngOnInit() {
        this.load();
    }

    public load(showInfo = false) {
        this.appNameOnce()
            .switchMap(app =>
                this.schemasService.getSchemas(app)
                    .combineLatest(this.webhooksService.getWebhooks(app),
                        (s, w) => { return { webhooks: w, schemas: s }; }))
            .subscribe(dtos => {
                this.schemas = dtos.schemas;
                this.webhooks = ImmutableArray.of(dtos.webhooks);

                if (showInfo) {
                    this.notifyInfo('Webhooks reloaded.');
                }
            }, error => {
                this.notifyError(error);
            });
    }

    public deleteWebhook(webhook: WebhookDto) {
        this.appNameOnce()
            .switchMap(app => this.webhooksService.deleteWebhook(app, webhook.id, webhook.version))
            .subscribe(dto => {
                this.webhooks = this.webhooks.remove(webhook);
            }, error => {
                this.notifyError(error);
            });
    }

    public updateWebhook(webhook: WebhookDto, requestDto: UpdateWebhookDto) {
        this.appNameOnce()
            .switchMap(app => this.webhooksService.putWebhook(app, webhook.id, requestDto, webhook.version))
            .subscribe(dto => {
                this.webhooks = this.webhooks.replace(webhook, webhook.update(requestDto, this.authService.user.token));

                this.notifyInfo('Webhook saved.');
            }, error => {
                this.notifyError(error);
            });
    }

    public addWebhook() {
        this.addWebhookFormSubmitted = true;

        if (this.addWebhookForm.valid) {
            this.addWebhookForm.disable();

            const requestDto = new CreateWebhookDto(this.addWebhookForm.controls['url'].value, []);

            const me = this.authService.user!.token;

            this.appNameOnce()
                .switchMap(app => this.webhooksService.postWebhook(app, requestDto, me, DateTime.now(), new Version()))
                .subscribe(dto => {
                    this.webhooks = this.webhooks.push(dto);

                    this.resetWebhookForm();
                }, error => {
                    this.notifyError(error);
                    this.enableWebhookForm();
                });
        }
    }

    public cancelAddWebhook() {
        this.resetWebhookForm();
    }

    private enableWebhookForm() {
        this.addWebhookForm.enable();
    }

    private resetWebhookForm() {
        this.addWebhookFormSubmitted = false;
        this.addWebhookForm.enable();
        this.addWebhookForm.reset();
    }
}
