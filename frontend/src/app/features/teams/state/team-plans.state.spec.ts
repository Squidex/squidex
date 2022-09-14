/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { of, throwError } from 'rxjs';
import { onErrorResumeNext } from 'rxjs/operators';
import { IMock, It, Mock, Times } from 'typemoq';
import { TeamPlansService, TeamPlansState } from '@app/features/teams/internal';
import { DialogService, PlanDto, PlanLockedReason, versioned } from '@app/shared';
import { TestValues } from '@app/shared/state/_test-helpers';

describe('TeamPlansState', () => {
    const {
        creator,
        newVersion,
        team,
        teamsState,
        version,
    } = TestValues;

    const oldPlans = {
        currentPlanId: 'id1',
        planOwner: creator,
        plans: [
            new PlanDto('id1', 'name1', '100€', undefined, 'id1_yearly', '200€', undefined, 1, 1, 1, 1),
            new PlanDto('id2', 'name2', '400€', undefined, 'id2_yearly', '800€', undefined, 2, 2, 2, 2),
        ],
        locked: 'None' as PlanLockedReason,
    };

    let dialogs: IMock<DialogService>;
    let plansService: IMock<TeamPlansService>;
    let plansState: TeamPlansState;

    beforeEach(() => {
        dialogs = Mock.ofType<DialogService>();

        plansService = Mock.ofType<TeamPlansService>();
        plansState = new TeamPlansState(teamsState.object, dialogs.object, plansService.object);
    });

    afterEach(() => {
        plansService.verifyAll();
    });

    describe('Loading', () => {
        it('should load plans', () => {
            plansService.setup(x => x.getPlans(team))
                .returns(() => of(versioned(version, oldPlans))).verifiable();

            plansState.load().subscribe();

            expect(plansState.snapshot.plans).toEqual([
                { isSelected: true, isYearlySelected: false, plan: oldPlans.plans[0] },
                { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[1] },
            ]);
            expect(plansState.snapshot.isLoaded).toBeTruthy();
            expect(plansState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should load plans with overriden id', () => {
            plansService.setup(x => x.getPlans(team))
                .returns(() => of(versioned(version, oldPlans))).verifiable();

            plansState.load(false, 'id2_yearly').subscribe();

            expect(plansState.snapshot.plans).toEqual([
                { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[0] },
                { isSelected: false, isYearlySelected: true, plan: oldPlans.plans[1] },
            ]);
            expect(plansState.snapshot.isLoaded).toBeTruthy();
            expect(plansState.snapshot.isLoading).toBeFalsy();
            expect(plansState.snapshot.version).toEqual(version);

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.never());
        });

        it('should reset loading state if loading failed', () => {
            plansService.setup(x => x.getPlans(team))
                .returns(() => throwError(() => 'Service Error'));

            plansState.load().pipe(onErrorResumeNext()).subscribe();

            expect(plansState.snapshot.isLoading).toBeFalsy();
        });

        it('should show notification on load if reload is true', () => {
            plansService.setup(x => x.getPlans(team))
                .returns(() => of(versioned(version, oldPlans))).verifiable();

            plansState.load(true).subscribe();

            expect().nothing();

            dialogs.verify(x => x.notifyInfo(It.isAnyString()), Times.once());
        });
    });

    describe('Updates', () => {
        beforeEach(() => {
            plansState.window = <any>{ location: {} };

            plansService.setup(x => x.getPlans(team))
                .returns(() => of(versioned(version, oldPlans))).verifiable();

            plansState.load().subscribe();
        });

        it('should redirect if returning url', () => {
            plansState.window = <any>{ location: {} };

            const result = { redirectUri: 'http://url' };

            plansService.setup(x => x.putPlan(team, It.isAny(), version))
                .returns(() => of(versioned(newVersion, result)));

            plansState.change('free').pipe(onErrorResumeNext()).subscribe();

            expect(plansState.snapshot.plans).toEqual([
                { isSelected: true, isYearlySelected: false, plan: oldPlans.plans[0] },
                { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[1] },
            ]);
            expect(plansState.window.location.href).toBe(result.redirectUri);
            expect(plansState.snapshot.version).toEqual(version);
        });

        it('should update plans if no returning url', () => {
            plansService.setup(x => x.putPlan(team, It.isAny(), version))
                .returns(() => of(versioned(newVersion, { redirectUri: '' })));

            plansState.change('id2_yearly').pipe(onErrorResumeNext()).subscribe();

            expect(plansState.snapshot.plans).toEqual([
                { isSelected: false, isYearlySelected: false, plan: oldPlans.plans[0] },
                { isSelected: false, isYearlySelected: true, plan: oldPlans.plans[1] },
            ]);
            expect(plansState.snapshot.version).toEqual(newVersion);
        });
    });
});
