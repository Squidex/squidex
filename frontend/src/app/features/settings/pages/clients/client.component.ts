/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppsState, ClientDto, ClientsState, ConfirmClickDirective, CopyDirective, DialogModel, EditableTitleComponent, FormHintComponent, ModalDirective, RoleDto, TourStepDirective, TranslatePipe, TypedSimpleChanges } from '@app/shared';
import { ClientConnectFormComponent } from './client-connect-form.component';

@Component({
    standalone: true,
    selector: 'sqx-client',
    styleUrls: ['./client.component.scss'],
    templateUrl: './client.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ClientConnectFormComponent,
        ConfirmClickDirective,
        CopyDirective,
        EditableTitleComponent,
        FormHintComponent,
        FormsModule,
        ModalDirective,
        NgFor,
        NgIf,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class ClientComponent {
    @Input({ required: true })
    public client!: ClientDto;

    @Input({ required: true })
    public clientRoles!: ReadonlyArray<RoleDto>;

    public apiCallsLimit = 0;

    public connectDialog = new DialogModel(false);

    constructor(
        public readonly appsState: AppsState,
        private readonly clientsState: ClientsState,
    ) {
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.client) {
            this.apiCallsLimit = this.client.apiCallsLimit;
        }
    }

    public revoke() {
        this.clientsState.revoke(this.client);
    }

    public updateRole(role: string) {
        this.clientsState.update(this.client, { role });
    }

    public updateAllowAnonymous(allowAnonymous: boolean) {
        this.clientsState.update(this.client, { allowAnonymous });
    }

    public updateApiCallsLimit() {
        this.clientsState.update(this.client, { apiCallsLimit: this.apiCallsLimit });
    }

    public rename(name: string) {
        this.clientsState.update(this.client, { name });
    }

    public trackByRole(_index: number, role: RoleDto) {
        return role.name;
    }
}
