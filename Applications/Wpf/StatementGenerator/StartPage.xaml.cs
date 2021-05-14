// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StartPage"/> class.
        /// </summary>
        public StartPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Click event of the btnStart control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnStart_Click( object sender, RoutedEventArgs e )
        {
            SelectPersonsPage nextPage = new SelectPersonsPage();
            this.NavigationService.Navigate( nextPage );
        }

        /// <summary>
        /// Handles the Click event of the mnuOptions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void mnuOptions_Click( object sender, RoutedEventArgs e )
        {
            OptionsPage optionsPage = new OptionsPage();
            this.NavigationService.Navigate( optionsPage );
        }

        /// <summary>
        /// Handles the Loaded event of the startPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void startPage_Loaded( object sender, RoutedEventArgs e )
        {
            ContributionReport.EnsureIncompletedSavedRecipientListCompletedStatus();
            var savedRecipientList = ContributionReport.GetSavedRecipientList();
            if ( savedRecipientList != null )
            {
                if ( savedRecipientList.Any( a => !a.IsComplete ) )
                {
                    pnlPromptToResume.Visibility = Visibility.Visible;
                    txtIntro.Visibility = Visibility.Collapsed;
                    return;
                }
            }

            pnlPromptToResume.Visibility = Visibility.Collapsed;
            txtIntro.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the Click event of the btnResumeNo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnResumeNo_Click( object sender, RoutedEventArgs e )
        {
            pnlPromptToResume.Visibility = Visibility.Collapsed;
            txtIntro.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Handles the Click event of the btnResumeYes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnResumeYes_Click( object sender, RoutedEventArgs e )
        {
            var nextPage = new ProgressPage();
            ContributionReport.Resume = true;
            ReportOptions.LoadFromConfig( RockConfig.Load() );
            this.NavigationService.Navigate( nextPage );
        }
    }
}
