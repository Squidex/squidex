/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ElementRef, EventEmitter, Output, ViewChild } from '@angular/core';
import { AppsState, AuthService, StatefulComponent, TranslationsService } from '@app/shared';

interface State {
    // True, when running
    isRunning: boolean;

    // The questions.
    chatQuestion: string;

    // The answers.
    chatTalk: ReadonlyArray<{ prompt: string; isUser?: boolean }>;
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
    public select = new EventEmitter<string>();

    @ViewChild('input', { static: false })
    public input!: ElementRef<HTMLInputElement>;

    public user = this.authService.user!;

    constructor(
        private readonly appsState: AppsState,
        private readonly authService: AuthService,
        private readonly translator: TranslationsService,
    ) {
        super({
            isRunning: false,
            chatQuestion: '',
            chatTalk: [],
        });
    }

    public setQuestion(chatQuestion: string) {
        this.next({ chatQuestion });
    }

    public ask() {
        const prompt = this.snapshot.chatQuestion;

        if (!prompt || prompt.length === 0) {
            return;
        }

        this.next(s => ({
            ...s,
            chatQuestion: '',
            chatTalk: [
                ...s.chatTalk,
                { prompt, isUser: true },
            ],
            isRunning: true,
        }));

        this.translator.ask(this.appsState.appName, { prompt })
            .subscribe({
                next: chatAnswers => {
                    this.next(s => ({
                        ...s,
                        chatTalk: [
                            ...s.chatTalk,
                            ...chatAnswers.map(answer => ({ prompt: answer })),
                        ],
                        isRunning: false,
                    }));

                    setTimeout(() => {
                        this.input.nativeElement.focus();
                    }, 100);
                },
                error: () => {
                    this.next({ isRunning: false });
                },
                complete: () => {
                    this.next({ isRunning: false });
                },
            });
    }
}