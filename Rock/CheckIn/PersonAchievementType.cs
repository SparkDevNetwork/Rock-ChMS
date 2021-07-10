using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonAchievementType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAchievementType"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="achievementType">Type of the achievement.</param>
        /// <param name="achievementAttempts">The achievement attempts.</param>
        /// <param name="justCompletedAchievementAttempt">The just completed achievement attempt.</param>
        public PersonAchievementType( Person person, AchievementTypeCache achievementType, AchievementAttempt[] achievementAttempts, AchievementAttempt justCompletedAchievementAttempt )
        {
            Person = person;
            AchievementType = achievementType;
            JustCompletedAchievementAttempt = justCompletedAchievementAttempt;

            CurrentInProgressAchievement = achievementAttempts.Where( a => !a.IsSuccessful && !a.IsClosed ).FirstOrDefault();
            AchievementAttempts = achievementAttempts;
        }

        /// <summary>
        /// Gets the type of the achievement.
        /// </summary>
        /// <value>
        /// The type of the achievement.
        /// </value>
        public AchievementTypeCache AchievementType { get; }

        /// <summary>
        /// Gets the just completed achievement attempt.
        /// </summary>
        /// <value>
        /// The just completed achievement attempt.
        /// </value>
        public AchievementAttempt JustCompletedAchievementAttempt { get; }

        /// <summary>
        /// Gets a value indicating whether [just completed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [just completed]; otherwise, <c>false</c>.
        /// </value>
        public bool JustCompleted => JustCompletedAchievementAttempt != null;

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public Person Person { get; }

        /// <summary>
        /// Gets the current in progress achievement.
        /// </summary>
        /// <value>
        /// The current in progress achievement.
        /// </value>
        public AchievementAttempt CurrentInProgressAchievement { get; }

        /// <summary>
        /// Gets the achievement attempts.
        /// </summary>
        /// <value>
        /// The achievement attempts.
        /// </value>
        public AchievementAttempt[] AchievementAttempts { get; }
    }
}
