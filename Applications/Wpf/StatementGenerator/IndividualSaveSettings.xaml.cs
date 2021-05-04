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
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// Interaction logic for SelectSavedLocationPage.xaml
    /// </summary>
    public partial class IndividualSaveSettings : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectSaveLocationPage"/> class.
        /// </summary>
        public IndividualSaveSettings()
        {
            InitializeComponent();
            var rockConfig = RockConfig.Load();

            Client.FinancialStatementIndividualSaveOptions saveOptions = null;

            try
            {
                if ( rockConfig.IndividualSaveOptionsJson.IsNotNullOrWhitespace() )
                {
                    saveOptions = Newtonsoft.Json.JsonConvert.DeserializeObject<Client.FinancialStatementIndividualSaveOptions>( rockConfig.IndividualSaveOptionsJson );
                }
            }
            catch
            {
                // ignore
            }

            saveOptions = saveOptions ?? new Client.FinancialStatementIndividualSaveOptions();


            cbSaveStatementsForIndividuals.IsChecked = saveOptions.SaveStatementsForIndividuals;
            cboDocumentType.SelectedValue = saveOptions.DocumentTypeId;
            txtDocumentDescription.Text = saveOptions.DocumentDescription;
            txtDocumentName.Text = saveOptions.DocumentName;
            txtDocumentPurposeKey.Text = saveOptions.DocumentPurposeKey;
            cbOverwriteDocumentsOfThisTypeCreatedOnSameDate.IsChecked = saveOptions.OverwriteDocumentsOfThisTypeCreatedOnSameDate;
            rbSaveForAllAdults.IsChecked = saveOptions.DocumentSaveFor == Client.Enums.FinancialStatementIndividualSaveOptionsSaveFor.AllActiveAdults;
            rbSaveForPrimaryGiver.IsChecked = saveOptions.DocumentSaveFor == Client.Enums.FinancialStatementIndividualSaveOptionsSaveFor.PrimaryGiver;
            rbSaveForAllActiveFamilyMembers.IsChecked = saveOptions.DocumentSaveFor == Client.Enums.FinancialStatementIndividualSaveOptionsSaveFor.AllActiveFamilyMembers;

            ReportOptions.Current.IndividualSaveOptions = saveOptions;
        }


        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="showWarnings">if set to <c>true</c> [show warnings].</param>
        /// <returns></returns>
        private bool SaveChanges( bool showWarnings )
        {

            var saveOptions = ReportOptions.Current.IndividualSaveOptions ?? new Client.FinancialStatementIndividualSaveOptions();
            saveOptions.SaveStatementsForIndividuals = cbSaveStatementsForIndividuals.IsChecked ?? false;
            saveOptions.DocumentTypeId = ( int? ) cboDocumentType.SelectedValue;
            saveOptions.DocumentDescription = txtDocumentDescription.Text;
            saveOptions.DocumentName = txtDocumentName.Text;
            saveOptions.DocumentPurposeKey = txtDocumentPurposeKey.Text;
            saveOptions.OverwriteDocumentsOfThisTypeCreatedOnSameDate = cbOverwriteDocumentsOfThisTypeCreatedOnSameDate.IsChecked ?? false;

            if ( rbSaveForAllAdults.IsChecked == true )
            {
                saveOptions.DocumentSaveFor = Client.Enums.FinancialStatementIndividualSaveOptionsSaveFor.AllActiveAdults;
            }
            else if ( rbSaveForPrimaryGiver.IsChecked == true )
            {
                saveOptions.DocumentSaveFor = Client.Enums.FinancialStatementIndividualSaveOptionsSaveFor.PrimaryGiver;
            }
            else if ( rbSaveForAllActiveFamilyMembers.IsChecked == true )
            {
                saveOptions.DocumentSaveFor = Client.Enums.FinancialStatementIndividualSaveOptionsSaveFor.AllActiveFamilyMembers;
            }

            ReportOptions.Current.IndividualSaveOptions = saveOptions;

            var rockConfig = RockConfig.Load();
            rockConfig.IndividualSaveOptionsJson = Newtonsoft.Json.JsonConvert.SerializeObject( saveOptions );
            rockConfig.Save();

            return true;
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnNext_Click( object sender, RoutedEventArgs e )
        {
            if ( SaveChanges( true ) )
            {
                var nextPage = new ProgressPage();
                this.NavigationService.Navigate( nextPage );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnPrev control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnPrev_Click( object sender, RoutedEventArgs e )
        {
            SaveChanges( false );
            this.NavigationService.GoBack();
        }

        private void cbSaveStatementsForIndividuals_Click( object sender, RoutedEventArgs e )
        {

        }
    }
}
