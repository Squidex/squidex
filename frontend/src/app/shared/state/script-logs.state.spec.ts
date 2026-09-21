/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, onErrorResumeNextWith, throwError } from 'rxjs';
import { IMock, It, Mock, Times } from 'typemoq';
import { DialogService, ScriptLogsService, ScriptLogsState } from '@app/shared/internal';
import { createScriptLog } from '../services/script-logs.service.spec';
import { TestValues } from './_test-helpers';

describe('ScriptLogsState', () => {
    const { app, appsState } = TestValues;

    const log1 = createScriptLog(12);
    const log2 = createScriptLog(13);

    let dialogs: IMock<DialogService>;
    let scriptLogsService: IMock<ScriptLogsService>;
    let scriptLogsState: ScriptLogsState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        scriptLogsService = Mock.ofType<ScriptLogsService>();
        scriptLogsState = new ScriptLogsState(appsState.object, scriptLogsService.object, dialogs.object);
    });

    afterEach(() => {
        scriptLogsService.verifyAll();
    });

    describe('Loading', () => {
        it('should load script logs', () => {
            scriptLogsService.setup(x => x.getScriptLogs(app, undefined))
                .returns(() => of({ items: [log1, log2] } as any)).verifiable();

            scriptLogsState.load().subscribe();

            expect(scriptLogsState.snapshot.scriptLogs).toEqual([log1, log2]);
            expect(scriptLogsState.snapshot.isLoaded).toBeTruthy();
            expect(scriptLogsState.snapshot.isLoading).toBeFalsy();
            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            scriptLogsService.setup(x => x.getScriptLogs(app, undefined))
                .returns(() => throwError(() => 'Service Error'));

            scriptLogsState.load().pipe(onErrorResumeNextWith()).subscribe();

            expect(scriptLogsState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            scriptLogsService.setup(x => x.getScriptLogs(app, undefined))
                .returns(() => of({ items: [log1, log2] } as any)).verifiable();

            scriptLogsState.load(true).subscribe();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });

        it('should load script logs with name filter', () => {
            scriptLogsService.setup(x => x.getScriptLogs(app, 'contents/my-schema'))
                .returns(() => of({ items: [log1] } as any)).verifiable();

            scriptLogsState.filter(' contents/my-schema ').subscribe();

            expect(scriptLogsState.snapshot.scriptLogs).toEqual([log1]);
            expect(scriptLogsState.snapshot.name).toEqual('contents/my-schema');
        });

        it('should keep name filter on reload', () => {
            scriptLogsService.setup(x => x.getScriptLogs(app, 'contents/my-schema'))
                .returns(() => of({ items: [log1] } as any)).verifiable(Times.exactly(2));

            scriptLogsState.filter('contents/my-schema').subscribe();
            scriptLogsState.load(true).subscribe();

            expect(scriptLogsState.snapshot.name).toEqual('contents/my-schema');
        });

        it('should remove name filter if empty', () => {
            scriptLogsService.setup(x => x.getScriptLogs(app, undefined))
                .returns(() => of({ items: [log1, log2] } as any)).verifiable();

            scriptLogsState.filter(' ').subscribe();

            expect(scriptLogsState.snapshot.name).toBeUndefined();
        });
    });
});
