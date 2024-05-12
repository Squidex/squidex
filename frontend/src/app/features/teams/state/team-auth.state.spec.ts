/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, onErrorResumeNextWith, throwError } from 'rxjs';
import { TestValues } from 'src/app/shared/state/_test-helpers';
import { IMock, It, Mock, Times } from 'typemoq';
import { AuthSchemeDto, DialogService, TeamsService, versioned } from '@app/shared';
import { TeamAuthState } from '../internal';

describe('TeamAuthState', () => {
    const {
        newVersion,
        team,
        teamsState,
        version,
    } = TestValues;

    const scheme: AuthSchemeDto = {
        domain: 'squidex.io',
        clientId: 'ID',
        clientSecret: 'secret',
        authority: 'Authority',
        displayName: 'Squidex',
    };

    let dialogs: IMock<DialogService>;
    let authService: IMock<TeamsService>;
    let authState: TeamAuthState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        authService = Mock.ofType<TeamsService>();
        authState = new TeamAuthState(teamsState.object, dialogs.object, authService.object);
    });

    afterEach(() => {
        authService.verifyAll();
    });

    describe('Loading', () => {
        it('should load auth', () => {
            authService.setup(x => x.getTeamAuth(team))
                .returns(() => of(versioned(version, { scheme, canUpdate: true }))).verifiable();

            authState.load().subscribe();

            expect(authState.snapshot.scheme).toEqual(scheme);
            expect(authState.snapshot.canUpdate).toBeTruthy();
            expect(authState.snapshot.isLoaded).toBeTruthy();
            expect(authState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            authService.setup(x => x.getTeamAuth(team))
                .returns(() => throwError(() => 'Service Error'));

            authState.load().pipe(onErrorResumeNextWith()).subscribe();

            expect(authState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            authService.setup(x => x.getTeamAuth(team))
                .returns(() => of(versioned(version, { scheme, canUpdate: true }))).verifiable();

            authState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            authService.setup(x => x.getTeamAuth(team))
                .returns(() => of(versioned(version, { scheme, canUpdate: true }))).verifiable();

            authState.load().subscribe();
        });

        it('should update scheme with new scheme', () => {
            const newScheme = { ...scheme, authority: 'NEW AUTHORIY' };

            authService.setup(x => x.putTeamAuth(team, It.isAny(), version))
                .returns(() => of(versioned(newVersion, { scheme: newScheme, canUpdate: true })));

            authState.update(newScheme);

            expect(authState.snapshot.scheme).toEqual(newScheme);
            expect(authState.snapshot.canUpdate).toBeTruthy();
            expect(authState.snapshot.version).toEqual(newVersion);
        });

        it('should update scheme with deleted scheme', () => {
            const newScheme = null;

            authService.setup(x => x.putTeamAuth(team, It.isAny(), version))
                .returns(() => of(versioned(newVersion, { scheme: newScheme, canUpdate: true })));

            authState.update(newScheme);

            expect(authState.snapshot.scheme).toEqual(newScheme);
            expect(authState.snapshot.canUpdate).toBeTruthy();
            expect(authState.snapshot.version).toEqual(newVersion);
        });
    });
});
