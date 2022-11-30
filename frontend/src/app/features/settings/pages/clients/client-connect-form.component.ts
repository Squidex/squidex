/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccessTokenDto, ApiUrlConfig, AppsState, ClientDto, ClientsService, DialogService } from '@app/shared';

@Component({
    selector: 'sqx-client-connect-form[client]',
    styleUrls: ['./client-connect-form.component.scss'],
    templateUrl: './client-connect-form.component.html',
})
export class ClientConnectFormComponent implements OnInit {
    @Output()
    public complete = new EventEmitter();

    @Input()
    public client!: ClientDto;

    public appName!: string;
    public appToken?: AccessTokenDto;

    public step = 'Start';

    public get isStart() {
        return this.step === 'Start';
    }

    constructor(
        public readonly appsState: AppsState,
        public readonly apiUrl: ApiUrlConfig,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly clientsService: ClientsService,
        private readonly dialogs: DialogService,
    ) {
    }

    public ngOnInit() {
        this.appName = this.appsState.appName;

        this.clientsService.createToken(this.appsState.appName, this.client)
            .subscribe({
                next: dto => {
                    this.appToken = dto;

                    this.changeDetector.detectChanges();
                },
                error: error => {
                    this.dialogs.notifyError(error);
                },
            });
    }

    public go(step: string) {
        this.step = step;
    }
}