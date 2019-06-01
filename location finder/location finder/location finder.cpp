#include <cassert>
#include <list>
#include <vector>
#include <stdio.h>
#include <stdlib.h>
#include <string>
#include <signal.h>
#include <fstream>
#include <iomanip>
#include <cstdlib>
#include<iostream>
using namespace std;

#ifdef _WIN32
#  define sleep(s) _sleep((s)*1000L)
#else
#  include <unistd.h>
#endif

#include "wpsapi.h"
#define TILES_TOTAL_DATA_SIZE (2 * 1024 * 1024) // 2 MB

static volatile bool interrupted = false;
static unsigned certified_iteration = 0;
static unsigned certified_iterations = 0;
static bool certified_output_json = false;
std::vector<unsigned char> salt;

static void
register_interrupt_handler(void(*handler)(int))
{
	signal(SIGTERM, handler);
	signal(SIGINT, handler);
#ifdef SIGHUP
	signal(SIGHUP, handler);
#endif
}

static void
interrupt_handler(int)
{
	if (!interrupted)
	{
		fprintf(stderr, "*** interrupted\n");
		interrupted = true;
	}

	register_interrupt_handler(interrupt_handler);
}

static void
print_street_address(const WPS_StreetAddress* address)
{
	if (!address)
		return;

	if (strlen(address->street_number) > 0)
		printf("%s ", address->street_number);

	char** line;
	for (line = address->address_line; *line; ++line)
		printf("%s\n", *line);

	printf("%s, %s %s\n",
		address->city,
		address->state.code,
		address->postal_code);
}

static void
print_location(const WPS_Location* location)
{
	printf("%f, %f\t+/-%.0fm\t%d+%d+%d  %lums\n",
		location->latitude,
		location->longitude,
		location->hpe,
		location->nap,
		location->ncell,
		location->nsat,
		location->age);

	if (location->speed >= 0)
	{
		printf("\t%.1fkm/h", location->speed);

		if (location->bearing >= 0)
			printf("\t%.0f", location->bearing);

		printf("\n");
	}

	if (location->hasScore != 0)
	{
		if (location->signature != NULL)
			printf("\tid: %d\n", location->id);

		printf("\tscore: %.1f\n", location->score);
		printf("\twith-ip: %s\n", location->withIP ? "true" : "false");

		if (location->ip != NULL)
			printf("\tip: %s\n", location->ip);

		printf("\ttimestamp: %lu\n", location->timestamp);
		printf("\thasSignature: %s\n", location->signature != NULL ? "true" : "false");
	}

	print_street_address(location->street_address);
}

static void
print_ip_location(const WPS_IPLocation* ip_location)
{
	printf("%s: %f, %f\n\n",
		ip_location->ip,
		ip_location->latitude,
		ip_location->longitude);
	print_street_address(ip_location->street_address);
}
static void
print_hex(const unsigned char* data,
unsigned length)
{
	for (unsigned i = 0; i < length; ++i)
		printf("%02x", data[i]);
}


static void
print_json_location(const WPS_Location* location)
{
	printf("\t{\n");
	printf("\t\t\"id\":\"%u\",\n", location->id);
	printf("\t\t\"latitude\":\"%f\",\n", location->latitude);
	printf("\t\t\"longitude\":\"%f\",\n", location->longitude);
	printf("\t\t\"hpe\":\"%d\",\n", (int)location->hpe);
	printf("\t\t\"nap\":\"%u\",\n", location->nap);
	printf("\t\t\"ncell\":\"%u\",\n", location->ncell);
	printf("\t\t\"nlac\":\"%u\",\n", location->nlac);
	printf("\t\t\"nsat\":\"%u\",\n", location->nsat);
	printf("\t\t\"nloc\":\"%u\",\n", location->historicalLocationCount);
	printf("\t\t\"age\":\"%lu\",\n", location->age);
	printf("\t\t\"score\":\"%f\",\n", location->score);
	printf("\t\t\"with-ip\":\"%s\",\n", location->withIP ? "true" : "false");
	printf("\t\t\"timestamp\":\"%lu\",\n", location->timestamp);
	printf("\t\t\"ip\":\"%s\",\n", location->ip);
	printf("\t\t\"hash\":\"");
	print_hex(location->hash, location->hashLength);
	printf("\",\n");
	printf("\t\t\"signature\":\"");
	print_hex(location->signature, location->signatureLength);
	printf("\"\n\t}");
}

static void
print_json_locations(const WPS_Location** locations,
unsigned nlocations)
{
	printf("\n{\n");
	printf("\t\"salt\":\"");
	print_hex(&salt[0], salt.size());
	printf("\",\n");
	printf("\t\"locations\":[\n");

	for (unsigned i = 0; i < nlocations; ++i)
	{
		if (i > 0)
			printf(",\n");

		print_json_location(locations[i]);
	}

	printf("]\n}\n");
}

static WPS_Continuation
certified_callback(void* arg,
WPS_ReturnCode code,
const WPS_Location** locations,
unsigned nlocations,
const void* reserved)
{
	if (interrupted)
		return WPS_STOP;

	if (code != WPS_OK)
	{
		fprintf(stderr, "*** failure (%d)!\n", code);

		if (code == WPS_ERROR_WIFI_NOT_AVAILABLE)
			return WPS_STOP;
	}
	else
	{
		for (unsigned i = 0; i < nlocations; ++i)
			print_location(locations[i]);

		if (certified_output_json && !salt.empty())
			print_json_locations(locations, nlocations);
	}

	if (++certified_iteration >= certified_iterations)
		return WPS_STOP;

	return WPS_CONTINUE;
}

static WPS_Continuation
periodic_callback(void*,
WPS_ReturnCode code,
const WPS_Location* location,
const void* measurements)
{
	if (interrupted)
		return WPS_STOP;

	if (code != WPS_OK)
	{
		fprintf(stderr, "*** failure (%d)!\n", code);

		if (code == WPS_ERROR_WIFI_NOT_AVAILABLE)
			return WPS_STOP;
	}
	else
	{
		print_location(location);
	}

	return WPS_CONTINUE;
}

static WPS_Continuation
tiling_callback(void* arg,
unsigned tileNumber,
unsigned tileTotal)
{
	unsigned maxTiles = arg ? *static_cast<unsigned*>(arg) : 0;

	if (maxTiles > 0 && tileNumber >= maxTiles)
		return WPS_STOP;
	fprintf(stderr, "downloading tile %d/%d...\n", tileNumber + 1, tileTotal);
	return WPS_CONTINUE;
}

/*********************************************************************/
/*                                                                   */
/* main                                                              */
/*                                                                   */
/*********************************************************************/

int
main(int argc, char* argv[])
{
	// parse arguments

	WPS_SimpleAuthentication authentication;
	const char* key = "eJwVwccNACAIAMC3w5BIkfJVYSnj7sY7bNg_Ce_tlItpeEBtNlhqAk6LIGbyIMHk3PcBEXALJg";
	authentication.username = "";
	authentication.realm = "";
	unsigned long period = 0;
	unsigned int iterations = 0, Iterations = 1;
	std::list<std::string> localfiles;
	const char* tiledir = NULL;
	unsigned maxtiles = 50;
	unsigned maxtotal = TILES_TOTAL_DATA_SIZE;
	unsigned maxsession = maxtotal / 5;
	WPS_ReturnCode rc;


	/*****************************************************************/
	/* WPS_load                                                      */
	/*****************************************************************/

	rc = WPS_load();
	if (rc != WPS_OK)
	{
		fprintf(stderr, "*** WPS_load failed (%d)!\n\n", rc);
		return rc;
	}

	/*****************************************************************/
	/* WPS_set_key / WPS_set_registration_user                       */
	/*****************************************************************/

	if (strlen(key) > 0)
	{
		rc = WPS_set_key(key);
		if (rc != WPS_OK)
			fprintf(stderr, "*** WPS_set_key failed (%d)!\n\n", rc);
	}
	else
	{
		rc = WPS_set_registration_user(&authentication);
		if (rc != WPS_OK)
			fprintf(stderr, "*** WPS_set_registration_user failed (%d)!\n\n", rc);
	}

	/*****************************************************************/
	/* WPS_set_tiling                                                */
	/*****************************************************************/

	// NOTE: WPS_set_tiling() must be called
	//       before WPS_periodic_location() or WPS_location()

	if (tiledir)
	{
		if (maxtiles > 0)
		{
			// limit the number of tiles downloaded,
			// around the current location,
			// to maxtiles via tiling_callback()

			rc = WPS_set_tiling(NULL,
				tiledir,
				maxtotal,
				maxtotal,
				tiling_callback,
				&maxtiles);
		}
		else
		{
			// limit the amount of tiles downloaded,
			// around the current location,
			// to (maxtotal / 5).

			rc = WPS_set_tiling(NULL,
				tiledir,
				maxsession,
				maxtotal,
				tiling_callback,
				NULL);
		}

		if (rc != WPS_OK)
			fprintf(stderr, "*** WPS_set_tiling failed (%d)!\n\n", rc);
	}

	/*****************************************************************/
	/* WPS_location                                                  */
	/*****************************************************************/

	if (period > 0 && Iterations > 0 && !interrupted)
	{
		for (unsigned i = 0; i < Iterations; ++i)
		{
			WPS_Location* location;
			rc = WPS_location(NULL,
				WPS_NO_STREET_ADDRESS_LOOKUP,
				&location);
			if (rc != WPS_OK)
			{
				fprintf(stderr, "*** WPS_location failed (%d)!\n\n", rc);
			}
			else
			{
				print_location(location);
				WPS_free_location(location);
			}

			sleep(period);
		}
	}
	else if (period == 0 && !interrupted)
	{
		WPS_Location* location;
		rc = WPS_location(NULL,
			WPS_FULL_STREET_ADDRESS_LOOKUP,
			&location);
		if (rc != WPS_OK)
		{
			fprintf(stderr, "*** WPS_location failed (%d)!\n\n", rc);
			string a = "latlon.txt";
			ofstream out_put(a.c_str(), ios::out);
			out_put <<"0" << "\n" <<"0";
			out_put.close();
		}
		else
		{
			print_location(location);
			cout << "creating file\n";
			string a = "latlon.txt";
			ofstream out_put(a.c_str(), ios::out);
			out_put << location->latitude << "\n" << location->longitude;
			out_put.close();
			WPS_free_location(location);

		}
	}


	/*****************************************************************/
	/* WPS_unload                                                    */
	/*****************************************************************/

	WPS_unload();
	system("pause");
	return 0;
}
