/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccessTokenDto, ApiUrlConfig, AppsState, ClientDto, ClientsService, ClientTourStated, DialogService, MessageBus } from '@app/shared';

@Component({
    selector: 'sqx-client-connect-form',
    styleUrls: ['./client-connect-form.component.scss'],
    templateUrl: './client-connect-form.component.html',
})
export class ClientConnectFormComponent implements OnInit {
    @Output()
    public close = new EventEmitter();

    @Input({ required: true })
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
        private readonly messageBus: MessageBus,
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

        this.messageBus.emit(new ClientTourStated());
    }

    public go(step: string) {
        this.step = step;
    }
}
