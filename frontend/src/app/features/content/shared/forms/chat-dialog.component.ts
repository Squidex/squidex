/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Component, EventEmitter, Output } from '@angular/core';
import { AppsState, StatefulComponent, TranslationsService } from '@app/shared';

interface State {
    // True, when running
    isRunning: boolean;

    // The questions.
    chatQuestion: string;

    // The answers.
    chatAnswers?: ReadonlyArray<string>;
}

@Component({
    selector: 'sqx-chat-dialog',
    styleUrls: ['./chat-dialog.component.scss'],
    templateUrl: './chat-dialog.component.html',
})
export class ChatDialogComponent extends StatefulComponent<State> {
    @Output()
    public close = new EventEmitter();

    @Output()
    public complete = new EventEmitter<string>();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly appsState: AppsState,
        private readonly translator: TranslationsService,
    ) {
        super(changeDetector, {
            isRunning: false,
            chatQuestion: '',
            chatAnswers: undefined,
        });
    }

    public setQuestion(chatQuestion: string) {
        this.next({ chatQuestion });
    }

    public ask() {
        this.next({ isRunning: true });

        this.translator.ask(this.appsState.appName, { prompt: this.snapshot.chatQuestion })
            .subscribe({
                next: chatAnswers => {
                    this.next({ chatAnswers, isRunning: false });
                },
                error: () => {
                    this.next({ chatAnswers: [], isRunning: false });
                },
                complete: () => {
                    this.next({ isRunning: false });
                },
            });
    }
}