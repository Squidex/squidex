/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { AppsState, ChatDialogComponent, DialogModel, ModalDirective, UIOptions } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-chat-menu',
    styleUrls: ['./chat-menu.component.scss'],
    templateUrl: './chat-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ChatDialogComponent,
        ModalDirective,
    ],
})
export class ChatMenuComponent {
    public readonly chatDialog = new DialogModel();

    public readonly hasChatBot = inject(UIOptions).value.canUseChatBot;

    constructor(
        public readonly appsState: AppsState,
    ) {
    }
}
